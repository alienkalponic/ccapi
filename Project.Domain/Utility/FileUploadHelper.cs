using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Utility
{
    public class FileUploadHelper
    {
        private static readonly string[] AllowedImageTypes =
        { "image/jpeg", "image/png", "image/webp", "image/svg+xml" };

        private static readonly string[] AllowedVideoTypes =
            { "video/mp4", "video/webm" };

        public static bool IsValidFile(IFormFile file)
        {
            return AllowedImageTypes.Contains(file.ContentType) ||
                   AllowedVideoTypes.Contains(file.ContentType);
        }

        public static bool IsImage(IFormFile file)
        {
            return AllowedImageTypes.Contains(file.ContentType);
        }

        public static async Task<(string fileUrl, string fileName)> SaveFileAsync(
            IFormFile file, string rootPath, string folderName)
        {
            string uploadsFolder = Path.Combine(rootPath, "assets/uploads/"+folderName);
            Directory.CreateDirectory(uploadsFolder);

            string extension = Path.GetExtension(file.FileName);
            string newFileName = $"{Guid.NewGuid()}{extension}";
            string fullPath = Path.Combine(uploadsFolder, newFileName);

            if (IsImage(file) && file.ContentType != "image/svg+xml")
            {

                newFileName = $"{Guid.NewGuid()}.webp";
                fullPath = Path.Combine(uploadsFolder, newFileName);

                using var image = await Image.LoadAsync(file.OpenReadStream());

                await image.SaveAsync(fullPath, new WebpEncoder
                {
                    Quality = 70
                });
            }
            else
            {
                using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);
            }

            string fileUrl = $"/assets/uploads/{folderName}/{newFileName}";
            return (fileUrl, newFileName);
        }

        public static async Task<string> SaveMobileVersionAsync(Image image, string path)
        {
            var clone = image.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(600, 0),   // width fixed, aspect auto
                Mode = ResizeMode.Max
            }));

            string mobilePath = Path.ChangeExtension(path, "_mobile.webp");

            await clone.SaveAsync(mobilePath, new WebpEncoder { Quality = 70 });

            return Path.GetFileName(mobilePath);
        }

        public static void DeleteFile(string webRootPath, string? fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return;

            try
            {
                var cleanUrl = fileUrl.Replace("/", Path.DirectorySeparatorChar.ToString())
                                      .TrimStart(Path.DirectorySeparatorChar);

                var fullPath = Path.Combine(webRootPath, cleanUrl);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                // TEMP DEBUG লগ করো
                Console.WriteLine($"DELETE ERROR: {ex.Message}");
            }
        }
    }
}
