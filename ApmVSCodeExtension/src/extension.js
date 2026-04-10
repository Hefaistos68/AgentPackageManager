const crypto = require('crypto');
const fs = require('fs');
const path = require('path');
const { execFile } = require('child_process');
const vscode = require('vscode');

const configFileName = 'agent-package.json';
const extensionConfigurationSection = 'apmVscodeExtension';

function activate(context) {
    context.subscriptions.push(
        vscode.commands.registerCommand('apmVscodeExtension.createWorkspaceAgentPackage', async () => {
            const workspaceFolder = await selectWorkspaceFolder();
            if (!workspaceFolder) {
                return;
            }

            await executePackagingAsync(workspaceFolder.uri.fsPath, path.basename(workspaceFolder.uri.fsPath), context.extensionPath);
        }),
        vscode.commands.registerCommand('apmVscodeExtension.createFolderAgentPackage', async resource => {
            const baseDirectory = await resolveFolderBaseDirectory(resource);
            if (!baseDirectory) {
                return;
            }

            await executePackagingAsync(baseDirectory, path.basename(baseDirectory), context.extensionPath);
        })
    );
}

async function executePackagingAsync(baseDirectory, defaultPackageId, extensionPath) {
    const githubDirectory = path.join(baseDirectory, '.github');
    if (!fs.existsSync(githubDirectory) || !fs.statSync(githubDirectory).isDirectory()) {
        await vscode.window.showErrorMessage(`No .github folder found in '${baseDirectory}'.`);
        return;
    }

    const outputDirectory = path.join(baseDirectory, 'bin', 'packages');
    const configPath = path.join(githubDirectory, configFileName);
    const config = readOrCreateConfig(configPath, defaultPackageId);
    const packageContentHash = computePackageContentHash(githubDirectory);
    const configForPrompt = { ...config };

    if (configForPrompt.contentHash !== packageContentHash) {
        configForPrompt.version = incrementPatchVersion(configForPrompt.version);
    }

    const updatedConfig = await promptForPackageConfig(configForPrompt);
    if (!updatedConfig) {
        return;
    }

    if (!areEquivalent(config, updatedConfig)) {
        writeConfig(configPath, updatedConfig);
    }

    try {
        await vscode.window.withProgress(
            {
                location: vscode.ProgressLocation.Notification,
                title: 'Creating agent package',
                cancellable: false
            },
            async () => {
                await runPackagerAsync(baseDirectory, outputDirectory, extensionPath);
            }
        );

        updatedConfig.contentHash = packageContentHash;
        writeConfig(configPath, updatedConfig);

        const packagePath = path.join(outputDirectory, `${updatedConfig.packageId}.${updatedConfig.version}.nupkg`);
        const selection = await vscode.window.showInformationMessage(`Package created successfully: ${packagePath}`, 'Reveal in Explorer');
        if (selection === 'Reveal in Explorer' && fs.existsSync(packagePath)) {
            await vscode.commands.executeCommand('revealFileInOS', vscode.Uri.file(packagePath));
        }
    }
    catch (error) {
        await vscode.window.showErrorMessage(getErrorMessage(error));
    }
}

function readOrCreateConfig(configPath, defaultPackageId) {
    if (fs.existsSync(configPath)) {
        const json = fs.readFileSync(configPath, 'utf8');
        let parsedConfig;

        try {
            parsedConfig = JSON.parse(json);
        }
        catch (error) {
            throw new Error(`Package config '${configPath}' is invalid: ${getErrorMessage(error)}`);
        }

        return normalizeConfig(parsedConfig, defaultPackageId);
    }

    const config = createDefaultConfig(defaultPackageId);
    writeConfig(configPath, config);
    return config;
}

function normalizeConfig(config, defaultPackageId) {
    const defaultConfig = createDefaultConfig(defaultPackageId);
    return {
        packageId: coerceString(config && config.packageId, defaultConfig.packageId),
        version: coerceString(config && config.version, defaultConfig.version),
        description: coerceString(config && config.description, defaultConfig.description),
        authors: coerceString(config && config.authors, defaultConfig.authors),
        contentHash: coerceString(config && config.contentHash, '')
    };
}

function createDefaultConfig(defaultPackageId) {
    return {
        packageId: `${defaultPackageId} Agent Configuration`,
        version: '1.0.0',
        description: 'GitHub Copilot assets package',
        authors: process.env.USERNAME || process.env.USER || 'unknown',
        contentHash: ''
    };
}

function computePackageContentHash(githubDirectory) {
    const hash = crypto.createHash('sha256');
    const packageSourceFiles = getPackageSourceFiles(githubDirectory);

    for (const sourceFile of packageSourceFiles) {
        const relativePath = getRelativePackagePath(githubDirectory, sourceFile);
        hash.update(relativePath, 'utf8');
        hash.update(Buffer.from([0]));
        hash.update(fs.readFileSync(sourceFile));
        hash.update(Buffer.from([0]));
    }

    return hash.digest('base64');
}

function getPackageSourceFiles(githubDirectory) {
    const files = [];
    enumerateFiles(githubDirectory, files);
    return files
        .filter(filePath => path.basename(filePath).toLowerCase() !== configFileName)
        .sort(comparePathsOrdinalIgnoreCase);
}

function enumerateFiles(currentDirectory, files) {
    const entries = fs.readdirSync(currentDirectory, { withFileTypes: true });
    for (const entry of entries) {
        const entryPath = path.join(currentDirectory, entry.name);
        if (entry.isDirectory()) {
            enumerateFiles(entryPath, files);
            continue;
        }

        if (entry.isFile()) {
            files.push(entryPath);
        }
    }
}

function comparePathsOrdinalIgnoreCase(left, right) {
    const leftUpper = left.toUpperCase();
    const rightUpper = right.toUpperCase();
    if (leftUpper < rightUpper) {
        return -1;
    }

    if (leftUpper > rightUpper) {
        return 1;
    }

    return 0;
}

function getRelativePackagePath(baseDirectory, targetPath) {
    return path.relative(baseDirectory, targetPath).split(path.sep).join('/');
}

function incrementPatchVersion(version) {
    const match = /^(\d+)\.(\d+)\.(\d+)(?:[-+].*)?$/.exec(version || '');
    if (!match) {
        return version || '1.0.0';
    }

    const major = Number.parseInt(match[1], 10);
    const minor = Number.parseInt(match[2], 10);
    const patch = Number.parseInt(match[3], 10) + 1;
    return `${major}.${minor}.${patch}`;
}

async function promptForPackageConfig(config) {
    const packageId = await vscode.window.showInputBox({
        title: 'Create Agent NuGet Package',
        prompt: 'Package ID',
        value: config.packageId,
        ignoreFocusOut: true
    });
    if (packageId === undefined) {
        return null;
    }

    const version = await vscode.window.showInputBox({
        title: 'Create Agent NuGet Package',
        prompt: 'Version',
        value: config.version,
        ignoreFocusOut: true
    });
    if (version === undefined) {
        return null;
    }

    const description = await vscode.window.showInputBox({
        title: 'Create Agent NuGet Package',
        prompt: 'Description',
        value: config.description,
        ignoreFocusOut: true
    });
    if (description === undefined) {
        return null;
    }

    const authors = await vscode.window.showInputBox({
        title: 'Create Agent NuGet Package',
        prompt: 'Authors',
        value: config.authors,
        ignoreFocusOut: true
    });
    if (authors === undefined) {
        return null;
    }

    return {
        packageId: packageId.trim(),
        version: version.trim(),
        description: description.trim(),
        authors: authors.trim(),
        contentHash: config.contentHash
    };
}

function areEquivalent(left, right) {
    return left.packageId === right.packageId
        && left.version === right.version
        && left.description === right.description
        && left.authors === right.authors
        && left.contentHash === right.contentHash;
}

function writeConfig(configPath, config) {
    fs.mkdirSync(path.dirname(configPath), { recursive: true });
    fs.writeFileSync(configPath, `${JSON.stringify(config, null, 2)}\n`, 'utf8');
}

async function runPackagerAsync(baseDirectory, outputDirectory, extensionPath) {
    const packagerPath = resolvePackagerPath(extensionPath);
    const packArguments = ['pack', '--source', baseDirectory, '--output', outputDirectory, '--force'];
    const commandSpec = createPackagerCommand(packagerPath, packArguments);
    await execFileAsync(commandSpec.command, commandSpec.args, baseDirectory);
}

function resolvePackagerPath(extensionPath) {
    const configuredPath = vscode.workspace.getConfiguration(extensionConfigurationSection).get('packagerPath', '').trim();
    if (configuredPath) {
        const workspaceRoot = vscode.workspace.workspaceFolders && vscode.workspace.workspaceFolders.length > 0
            ? vscode.workspace.workspaceFolders[0].uri.fsPath
            : extensionPath;
        const resolvedPath = path.isAbsolute(configuredPath)
            ? configuredPath
            : path.resolve(workspaceRoot, configuredPath);

        if (!fs.existsSync(resolvedPath)) {
            throw new Error(`Configured packager path '${resolvedPath}' does not exist.`);
        }

        return resolvedPath;
    }

    const candidatePaths = [
        path.resolve(extensionPath, '..', 'ApmPackager', 'ApmPackager.csproj'),
        path.resolve(extensionPath, 'ApmPackager', 'ApmPackager.csproj')
    ];

    for (const candidatePath of candidatePaths) {
        if (fs.existsSync(candidatePath)) {
            return candidatePath;
        }
    }

    throw new Error('Unable to locate ApmPackager. Configure apmVscodeExtension.packagerPath to point to ApmPackager.csproj, ApmPackager.dll, or a native executable.');
}

function createPackagerCommand(packagerPath, packArguments) {
    const extension = path.extname(packagerPath).toLowerCase();
    if (extension === '.csproj') {
        return {
            command: 'dotnet',
            args: ['run', '--project', packagerPath, '--', ...packArguments]
        };
    }

    if (extension === '.dll') {
        return {
            command: 'dotnet',
            args: [packagerPath, ...packArguments]
        };
    }

    return {
        command: packagerPath,
        args: packArguments
    };
}

function execFileAsync(command, args, workingDirectory) {
    return new Promise((resolve, reject) => {
        execFile(
            command,
            args,
            {
                cwd: workingDirectory,
                windowsHide: true,
                maxBuffer: 10 * 1024 * 1024
            },
            (error, stdout, stderr) => {
                if (error) {
                    reject(new Error((stderr || stdout || error.message).trim()));
                    return;
                }

                resolve({ stdout, stderr });
            }
        );
    });
}

async function selectWorkspaceFolder() {
    const workspaceFolders = vscode.workspace.workspaceFolders || [];
    if (workspaceFolders.length === 0) {
        await vscode.window.showErrorMessage('Open a workspace folder first.');
        return null;
    }

    if (workspaceFolders.length === 1) {
        return workspaceFolders[0];
    }

    return vscode.window.showWorkspaceFolderPick({
        placeHolder: 'Select the workspace folder that contains the .github folder to package.'
    });
}

async function resolveFolderBaseDirectory(resource) {
    if (resource && resource.fsPath) {
        const resourcePath = resource.fsPath;
        if (fs.existsSync(resourcePath) && fs.statSync(resourcePath).isDirectory()) {
            return resourcePath;
        }

        return path.dirname(resourcePath);
    }

    const workspaceFolder = await selectWorkspaceFolder();
    return workspaceFolder ? workspaceFolder.uri.fsPath : null;
}

function coerceString(value, fallback) {
    return typeof value === 'string' ? value : fallback;
}

function getErrorMessage(error) {
    if (error instanceof Error && error.message) {
        return error.message;
    }

    return String(error);
}

function deactivate() {
}

module.exports = {
    activate,
    deactivate,
    // Exported for unit testing
    incrementPatchVersion,
    comparePathsOrdinalIgnoreCase,
    getRelativePackagePath,
    coerceString,
    getErrorMessage,
    areEquivalent,
    createDefaultConfig,
    normalizeConfig,
    createPackagerCommand,
    writeConfig,
    readOrCreateConfig,
    computePackageContentHash,
    getPackageSourceFiles
};
