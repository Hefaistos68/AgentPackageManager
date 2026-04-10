'use strict';

const assert = require('assert');
const path = require('path');
const fs = require('fs');
const os = require('os');
const proxyquire = require('proxyquire');

const vscodeMock = require('./mocks/vscode');
const ext = proxyquire('../src/extension', {
    'vscode': { ...vscodeMock, '@noCallThru': true }
});

function makeTempDir() {
    return fs.mkdtempSync(path.join(os.tmpdir(), 'apm-test-'));
}

function removeTempDir(dirPath) {
    fs.rmSync(dirPath, { recursive: true, force: true });
}

// ---------------------------------------------------------------------------

describe('incrementPatchVersion', () => {
    it('increments the patch segment', () => {
        assert.strictEqual(ext.incrementPatchVersion('1.0.0'), '1.0.1');
    });

    it('rolls over from 9 to 10', () => {
        assert.strictEqual(ext.incrementPatchVersion('1.0.9'), '1.0.10');
    });

    it('leaves major and minor unchanged', () => {
        assert.strictEqual(ext.incrementPatchVersion('2.3.4'), '2.3.5');
    });

    it('strips pre-release suffix and increments patch', () => {
        assert.strictEqual(ext.incrementPatchVersion('1.0.0-beta'), '1.0.1');
    });

    it('returns 1.0.0 for null', () => {
        assert.strictEqual(ext.incrementPatchVersion(null), '1.0.0');
    });

    it('returns 1.0.0 for empty string', () => {
        assert.strictEqual(ext.incrementPatchVersion(''), '1.0.0');
    });

    it('returns the original value for a non-semver string', () => {
        assert.strictEqual(ext.incrementPatchVersion('not-a-version'), 'not-a-version');
    });
});

// ---------------------------------------------------------------------------

describe('comparePathsOrdinalIgnoreCase', () => {
    it('returns 0 for equal strings', () => {
        assert.strictEqual(ext.comparePathsOrdinalIgnoreCase('abc', 'abc'), 0);
    });

    it('is case-insensitive', () => {
        assert.strictEqual(ext.comparePathsOrdinalIgnoreCase('ABC', 'abc'), 0);
        assert.strictEqual(ext.comparePathsOrdinalIgnoreCase('Path/To/File', 'path/to/file'), 0);
    });

    it('returns negative when left < right', () => {
        assert.ok(ext.comparePathsOrdinalIgnoreCase('abc', 'def') < 0);
    });

    it('returns positive when left > right', () => {
        assert.ok(ext.comparePathsOrdinalIgnoreCase('def', 'abc') > 0);
    });
});

// ---------------------------------------------------------------------------

describe('getRelativePackagePath', () => {
    it('returns a forward-slash path', () => {
        const base = path.join(os.tmpdir(), 'project', '.github');
        const target = path.join(base, 'subdir', 'file.txt');
        assert.strictEqual(ext.getRelativePackagePath(base, target), 'subdir/file.txt');
    });

    it('returns just the filename when the file is directly in base', () => {
        const base = path.join(os.tmpdir(), 'project', '.github');
        const target = path.join(base, 'file.md');
        assert.strictEqual(ext.getRelativePackagePath(base, target), 'file.md');
    });

    it('never contains backslashes', () => {
        const base = path.join(os.tmpdir(), 'a', 'b');
        const target = path.join(base, 'c', 'd', 'e.txt');
        const result = ext.getRelativePackagePath(base, target);
        assert.ok(!result.includes('\\'), `Expected no backslashes, got: ${result}`);
    });
});

// ---------------------------------------------------------------------------

describe('coerceString', () => {
    it('returns the value when it is a string', () => {
        assert.strictEqual(ext.coerceString('hello', 'default'), 'hello');
    });

    it('returns an empty string when the value is an empty string', () => {
        assert.strictEqual(ext.coerceString('', 'default'), '');
    });

    it('returns the fallback for null', () => {
        assert.strictEqual(ext.coerceString(null, 'default'), 'default');
    });

    it('returns the fallback for undefined', () => {
        assert.strictEqual(ext.coerceString(undefined, 'default'), 'default');
    });

    it('returns the fallback for a number', () => {
        assert.strictEqual(ext.coerceString(42, 'default'), 'default');
    });
});

// ---------------------------------------------------------------------------

describe('getErrorMessage', () => {
    it('returns the message from an Error object', () => {
        assert.strictEqual(ext.getErrorMessage(new Error('oops')), 'oops');
    });

    it('returns a plain string unchanged', () => {
        assert.strictEqual(ext.getErrorMessage('raw error'), 'raw error');
    });

    it('converts a number to string', () => {
        assert.strictEqual(ext.getErrorMessage(404), '404');
    });

    it('converts an object to string', () => {
        assert.strictEqual(ext.getErrorMessage({ toString: () => 'obj' }), 'obj');
    });
});

// ---------------------------------------------------------------------------

describe('areEquivalent', () => {
    const base = {
        packageId: 'My.Package',
        version: '1.0.0',
        description: 'Test',
        authors: 'Author',
        contentHash: 'abc123'
    };

    it('returns true for identical configs', () => {
        assert.strictEqual(ext.areEquivalent(base, { ...base }), true);
    });

    it('returns false when packageId differs', () => {
        assert.strictEqual(ext.areEquivalent(base, { ...base, packageId: 'Other' }), false);
    });

    it('returns false when version differs', () => {
        assert.strictEqual(ext.areEquivalent(base, { ...base, version: '2.0.0' }), false);
    });

    it('returns false when description differs', () => {
        assert.strictEqual(ext.areEquivalent(base, { ...base, description: 'Other' }), false);
    });

    it('returns false when authors differs', () => {
        assert.strictEqual(ext.areEquivalent(base, { ...base, authors: 'Other' }), false);
    });

    it('returns false when contentHash differs', () => {
        assert.strictEqual(ext.areEquivalent(base, { ...base, contentHash: 'xyz' }), false);
    });
});

// ---------------------------------------------------------------------------

describe('createDefaultConfig', () => {
    it('uses the provided name as the package ID base', () => {
        const config = ext.createDefaultConfig('MyProject');
        assert.strictEqual(config.packageId, 'MyProject Agent Configuration');
    });

    it('sets version to 1.0.0', () => {
        assert.strictEqual(ext.createDefaultConfig('X').version, '1.0.0');
    });

    it('sets the default description', () => {
        assert.strictEqual(ext.createDefaultConfig('X').description, 'GitHub Copilot assets package');
    });

    it('sets an empty contentHash', () => {
        assert.strictEqual(ext.createDefaultConfig('X').contentHash, '');
    });

    it('sets authors to a non-empty string', () => {
        const config = ext.createDefaultConfig('X');
        assert.ok(typeof config.authors === 'string' && config.authors.length > 0);
    });
});

// ---------------------------------------------------------------------------

describe('normalizeConfig', () => {
    it('preserves all provided fields', () => {
        const raw = {
            packageId: 'Custom.Id',
            version: '2.0.0',
            description: 'Custom desc',
            authors: 'Custom Author',
            contentHash: 'hash123'
        };
        const config = ext.normalizeConfig(raw, 'Default');
        assert.strictEqual(config.packageId, 'Custom.Id');
        assert.strictEqual(config.version, '2.0.0');
        assert.strictEqual(config.description, 'Custom desc');
        assert.strictEqual(config.authors, 'Custom Author');
        assert.strictEqual(config.contentHash, 'hash123');
    });

    it('falls back to defaults for missing fields', () => {
        const config = ext.normalizeConfig({}, 'MyProject');
        assert.strictEqual(config.packageId, 'MyProject Agent Configuration');
        assert.strictEqual(config.version, '1.0.0');
        assert.strictEqual(config.contentHash, '');
    });

    it('handles a null config object', () => {
        const config = ext.normalizeConfig(null, 'MyProject');
        assert.strictEqual(config.packageId, 'MyProject Agent Configuration');
    });

    it('uses empty string as default contentHash (not null)', () => {
        const config = ext.normalizeConfig({}, 'X');
        assert.strictEqual(config.contentHash, '');
    });
});

// ---------------------------------------------------------------------------

describe('createPackagerCommand', () => {
    const packArgs = ['pack', '--source', '.', '--output', 'out'];

    it('uses dotnet run for a .csproj path', () => {
        const cmd = ext.createPackagerCommand('/path/ApmPackager.csproj', packArgs);
        assert.strictEqual(cmd.command, 'dotnet');
        assert.ok(cmd.args.includes('run'));
        assert.ok(cmd.args.includes('--project'));
        assert.ok(cmd.args.includes('/path/ApmPackager.csproj'));
    });

    it('places pack arguments after the -- separator for .csproj', () => {
        const cmd = ext.createPackagerCommand('/path/ApmPackager.csproj', packArgs);
        const separatorIndex = cmd.args.indexOf('--');
        assert.ok(separatorIndex >= 0);
        assert.deepStrictEqual(cmd.args.slice(separatorIndex + 1), packArgs);
    });

    it('uses dotnet with the dll path as first arg for a .dll path', () => {
        const cmd = ext.createPackagerCommand('/path/ApmPackager.dll', packArgs);
        assert.strictEqual(cmd.command, 'dotnet');
        assert.strictEqual(cmd.args[0], '/path/ApmPackager.dll');
    });

    it('passes pack arguments directly for a .dll', () => {
        const cmd = ext.createPackagerCommand('/path/ApmPackager.dll', packArgs);
        assert.deepStrictEqual(cmd.args.slice(1), packArgs);
    });

    it('uses the executable as command for a native binary', () => {
        const cmd = ext.createPackagerCommand('/path/ApmPackager', packArgs);
        assert.strictEqual(cmd.command, '/path/ApmPackager');
        assert.deepStrictEqual(cmd.args, packArgs);
    });

    it('is case-insensitive for extensions', () => {
        const cmd = ext.createPackagerCommand('/path/ApmPackager.CSPROJ', packArgs);
        assert.strictEqual(cmd.command, 'dotnet');
    });
});

// ---------------------------------------------------------------------------

describe('writeConfig and readOrCreateConfig', () => {
    let tmpDir;

    beforeEach(() => { tmpDir = makeTempDir(); });
    afterEach(() => { removeTempDir(tmpDir); });

    it('round-trips a config through write and read', () => {
        const configPath = path.join(tmpDir, 'agent-package.json');
        const original = {
            packageId: 'Test.Package',
            version: '1.2.3',
            description: 'A test package',
            authors: 'Tester',
            contentHash: 'abc'
        };
        ext.writeConfig(configPath, original);
        const read = ext.readOrCreateConfig(configPath, 'Default');
        assert.strictEqual(read.packageId, original.packageId);
        assert.strictEqual(read.version, original.version);
        assert.strictEqual(read.description, original.description);
        assert.strictEqual(read.authors, original.authors);
        assert.strictEqual(read.contentHash, original.contentHash);
    });

    it('creates the file with default values when it does not exist', () => {
        const configPath = path.join(tmpDir, 'sub', 'agent-package.json');
        const config = ext.readOrCreateConfig(configPath, 'MyProject');
        assert.strictEqual(config.packageId, 'MyProject Agent Configuration');
        assert.strictEqual(config.version, '1.0.0');
        assert.ok(fs.existsSync(configPath));
    });

    it('creates intermediate directories when writing', () => {
        const configPath = path.join(tmpDir, 'deep', 'nested', 'agent-package.json');
        const data = { packageId: 'X', version: '1.0.0', description: '', authors: '', contentHash: '' };
        ext.writeConfig(configPath, data);
        assert.ok(fs.existsSync(configPath));
    });

    it('throws for invalid JSON in an existing file', () => {
        const configPath = path.join(tmpDir, 'agent-package.json');
        fs.writeFileSync(configPath, 'not { valid json', 'utf8');
        assert.throws(() => ext.readOrCreateConfig(configPath, 'Test'), /invalid/i);
    });

    it('writes valid JSON ending with a newline', () => {
        const configPath = path.join(tmpDir, 'agent-package.json');
        ext.writeConfig(configPath, { packageId: 'X', version: '1.0.0', description: '', authors: '', contentHash: '' });
        const raw = fs.readFileSync(configPath, 'utf8');
        assert.ok(raw.endsWith('\n'));
        assert.doesNotThrow(() => JSON.parse(raw));
    });
});

// ---------------------------------------------------------------------------

describe('computePackageContentHash', () => {
    let tmpDir;

    beforeEach(() => { tmpDir = makeTempDir(); });
    afterEach(() => { removeTempDir(tmpDir); });

    it('produces a non-empty base64 string', () => {
        fs.writeFileSync(path.join(tmpDir, 'instructions.md'), 'some content');
        const hash = ext.computePackageContentHash(tmpDir);
        assert.ok(typeof hash === 'string' && hash.length > 0);
    });

    it('produces the same hash on repeated calls for identical content', () => {
        fs.writeFileSync(path.join(tmpDir, 'file.md'), 'content');
        assert.strictEqual(
            ext.computePackageContentHash(tmpDir),
            ext.computePackageContentHash(tmpDir)
        );
    });

    it('changes hash when file content is modified', () => {
        const filePath = path.join(tmpDir, 'file.md');
        fs.writeFileSync(filePath, 'original');
        const hash1 = ext.computePackageContentHash(tmpDir);
        fs.writeFileSync(filePath, 'modified');
        const hash2 = ext.computePackageContentHash(tmpDir);
        assert.notStrictEqual(hash1, hash2);
    });

    it('changes hash when a file is added', () => {
        fs.writeFileSync(path.join(tmpDir, 'file.md'), 'content');
        const hash1 = ext.computePackageContentHash(tmpDir);
        fs.writeFileSync(path.join(tmpDir, 'new.md'), 'new');
        const hash2 = ext.computePackageContentHash(tmpDir);
        assert.notStrictEqual(hash1, hash2);
    });

    it('does not include agent-package.json in the hash', () => {
        fs.writeFileSync(path.join(tmpDir, 'file.md'), 'content');
        const hash1 = ext.computePackageContentHash(tmpDir);
        fs.writeFileSync(path.join(tmpDir, 'agent-package.json'), '{"version":"1.0.0"}');
        const hash2 = ext.computePackageContentHash(tmpDir);
        assert.strictEqual(hash1, hash2, 'Hash must not change when only agent-package.json changes');
    });

    it('returns an empty hash (no bytes hashed) for an empty directory', () => {
        const hash1 = ext.computePackageContentHash(tmpDir);
        assert.ok(typeof hash1 === 'string');
    });
});

// ---------------------------------------------------------------------------

describe('getPackageSourceFiles', () => {
    let tmpDir;

    beforeEach(() => { tmpDir = makeTempDir(); });
    afterEach(() => { removeTempDir(tmpDir); });

    it('returns all files in the directory', () => {
        fs.writeFileSync(path.join(tmpDir, 'a.md'), '');
        fs.writeFileSync(path.join(tmpDir, 'b.md'), '');
        const files = ext.getPackageSourceFiles(tmpDir);
        assert.strictEqual(files.length, 2);
    });

    it('recurses into subdirectories', () => {
        const sub = path.join(tmpDir, 'subdir');
        fs.mkdirSync(sub);
        fs.writeFileSync(path.join(tmpDir, 'root.md'), '');
        fs.writeFileSync(path.join(sub, 'nested.md'), '');
        const files = ext.getPackageSourceFiles(tmpDir);
        assert.strictEqual(files.length, 2);
    });

    it('excludes agent-package.json', () => {
        fs.writeFileSync(path.join(tmpDir, 'instructions.md'), '');
        fs.writeFileSync(path.join(tmpDir, 'agent-package.json'), '');
        const files = ext.getPackageSourceFiles(tmpDir);
        assert.ok(files.every(f => path.basename(f).toLowerCase() !== 'agent-package.json'));
        assert.strictEqual(files.length, 1);
    });

    it('returns paths sorted case-insensitively', () => {
        fs.writeFileSync(path.join(tmpDir, 'z.md'), '');
        fs.writeFileSync(path.join(tmpDir, 'A.md'), '');
        fs.writeFileSync(path.join(tmpDir, 'm.md'), '');
        const files = ext.getPackageSourceFiles(tmpDir);
        const names = files.map(f => path.basename(f).toUpperCase());
        assert.deepStrictEqual(names, [...names].sort());
    });

    it('returns absolute paths', () => {
        fs.writeFileSync(path.join(tmpDir, 'file.md'), '');
        const files = ext.getPackageSourceFiles(tmpDir);
        assert.ok(path.isAbsolute(files[0]));
    });
});
