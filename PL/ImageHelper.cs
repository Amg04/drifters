namespace PL
{
    public static class ImageHelper
    {
        public static string SaveImage(IFormFile imageFile, IWebHostEnvironment hostEnvironment)
        {
            var webRootPath = hostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadPath = Path.Combine(webRootPath, "api", "uploads");
            Directory.CreateDirectory(uploadPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(fileStream);
            }

            return $"/api/uploads/{fileName}";
        }

        public static void DeleteImage(string imageUrl, IWebHostEnvironment hostEnvironment)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var webRootPath = hostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var filePath = Path.Combine(webRootPath, "api", "uploads", Path.GetFileName(imageUrl));

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }
    }
}
