using System.ComponentModel.DataAnnotations;

namespace Trucktaxpro.Models;

public class VehicleUploadViewModel
{
    public int BusinessId { get; set; }

    [Required(ErrorMessage = "Please choose a file to upload.")]
    public IFormFile File { get; set; } = null!;
}