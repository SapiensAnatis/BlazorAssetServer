using System.ComponentModel.DataAnnotations;
using AssetServer.Validation;

namespace AssetServer.Models;

public class ConfigurationModel
{
    [ValidServerPath(
        ErrorMessage = "Android path must be a valid path to a directory on the server."
    )]
    public string AndroidFilepath { get; set; } = string.Empty;

    [ValidServerPath(ErrorMessage = "iOS path must be a valid path to a directory on the server.")]
    public string IosFilepath { get; set; } = string.Empty;

    [ValidServerPath(
        ErrorMessage = "Manifest path must be a valid path to a directory on the server."
    )]
    [Required]
    public string ManifestPath { get; set; } = string.Empty;

    public bool EnableOverride { get; set; }
}
