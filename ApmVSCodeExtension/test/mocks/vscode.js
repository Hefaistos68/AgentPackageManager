'use strict';

const vscode = {
    commands: {
        registerCommand: () => ({ dispose: () => {} }),
        executeCommand: async () => {}
    },
    window: {
        showErrorMessage: async () => undefined,
        showInformationMessage: async () => undefined,
        showInputBox: async () => undefined,
        showWorkspaceFolderPick: async () => undefined,
        withProgress: async (_options, task) => task({ report: () => {} })
    },
    workspace: {
        getConfiguration: () => ({ get: (_key, defaultValue) => defaultValue }),
        workspaceFolders: []
    },
    ProgressLocation: {
        Notification: 15
    },
    Uri: {
        file: fsPath => ({ fsPath, scheme: 'file' })
    }
};

module.exports = vscode;
