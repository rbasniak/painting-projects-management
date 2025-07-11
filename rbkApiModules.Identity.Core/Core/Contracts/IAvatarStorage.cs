﻿namespace rbkApiModules.Identity.Core;

public interface IAvatarStorage
{
    string GetRelativePath(string filename);
    string GetAbsolutePath(string path);
    Task<string> SaveAsync(string base64Data, string path, string filenameWithoutExtension, string extension, CancellationToken cancellationToken);
    Task<string> SaveAsync(string base64Data, string path, string filenameWithoutExtension, CancellationToken cancellationToken);
}

