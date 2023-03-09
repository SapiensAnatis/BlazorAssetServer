using System.ComponentModel.DataAnnotations;
using AssetServer.Models;
using AssetServer.Services;
using AssetServer.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AssetServer;

[ApiController]
[Route("/dl/assetbundles")]
public class AssetController : ControllerBase
{
    private const int MatchTimeout = 1000;
    private const string PlatformRegex = "iOS|Android";
    private const string AssetPrefixRegex = @"[A-Z0-9]{2,}";
    private const string AssetNameRegex = @"[A-Z0-9]{52,}";

    private readonly ConfigurationService configurationService;
    private readonly FileRetrievalService fileRetrievalService;

    public AssetController(
        ConfigurationService configurationService,
        FileRetrievalService fileRetrievalService
    )
    {
        this.configurationService = configurationService;
        this.fileRetrievalService = fileRetrievalService;
    }

    [HttpGet("{platform}/{assetPrefix}/{assetName}")]
    [Produces("application/octet-stream")]
    public IActionResult GetAsset(
        [FromRoute]
        [RegularExpression(PlatformRegex, MatchTimeoutInMilliseconds = MatchTimeout)]
            string platform,
        [FromRoute]
        [RegularExpression(AssetPrefixRegex, MatchTimeoutInMilliseconds = MatchTimeout)]
            string assetPrefix,
        [FromRoute]
        [RegularExpression(AssetNameRegex, MatchTimeoutInMilliseconds = MatchTimeout)]
            string assetName
    )
    {
        string basePath = platform switch
        {
            "Android" => this.configurationService.CurrentValue.AndroidFilepath,
            "iOS" => this.configurationService.CurrentValue.IosFilepath,
            _ => throw new BadHttpRequestException("Unrecognized platform")
        };

        string path = Path.Join(basePath, assetPrefix, assetName);

        if (!this.fileRetrievalService.TryLoadFile(path, out FileStream? stream))
            return NotFound();

        return File(stream, "application/octet-stream");
    }
}
