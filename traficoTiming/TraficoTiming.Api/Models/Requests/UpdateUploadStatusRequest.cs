using TraficoTiming.Database.Enums;

namespace TraficoTiming.Api.Models.Requests;

public class UpdateUploadStatusRequest
{
    public UploadStatus UploadStatus { get; set; }
}
