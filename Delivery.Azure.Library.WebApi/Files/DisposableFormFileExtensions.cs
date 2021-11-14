using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Delivery.Azure.Library.WebApi.Files
{
    public static class DisposableFormFileExtensions
    {
        /// <summary>
		///     Gets a list of multipart files if they have been sent in the request
		/// </summary>
		public static async Task<ReadOnlyCollection<DisposableFormFile>> GetFilesAsync(this HttpRequest httpRequest)
		{
			var boundary = httpRequest.GetMultipartBoundary();
			var reader = new MultipartReader(boundary, httpRequest.Body);

			var sections = new List<MultipartSection>();
			var section = await reader.ReadNextSectionAsync();
			while (section != null)
			{
				var contentDispositionHeaderValue = section.GetContentDispositionHeader();
				if (contentDispositionHeaderValue?.IsFileDisposition() ?? false)
				{
					sections.Add(section);
				}

				section = await reader.ReadNextSectionAsync();
			}

			var tasks = new List<Task<DisposableFormFile>>();
			sections.ForEach(p => tasks.Add(GetFormFileAsync(p)));
			await Task.WhenAll(tasks);

			return tasks.Select(p => p.Result).ToList().AsReadOnly();
		}

		/// <summary>
		///  Gets a file if they have been sent in the request
		/// </summary>
		public static async Task<DisposableFormFile> GetBinaryFileAsync(this HttpRequest httpRequest, string fileName)
		{
			const int bufferSize = 32 * 1024;
			await ReadStreamAsync(httpRequest.Body, bufferSize);
			httpRequest.Body.Position = 0;
			var contentType = httpRequest.ContentType;
			var formFile = new DisposableFormFile(httpRequest.Body, baseStreamOffset: 0, httpRequest.Body.Length , fileName, fileName)
			{
				Headers = new HeaderDictionary(),
				ContentType = contentType,
				ContentDisposition = httpRequest.Body.Position.ToString()
			};

			return formFile;
		}

		private static async Task<DisposableFormFile> GetFormFileAsync(MultipartSection section)
		{
			var fileSection = section.AsFileSection();
			if (fileSection?.FileStream == null)
			{
				throw new ValidationException("Stream body cannot be read; ensure that the uploaded data is well formatted");
			}

			const int bufferSize = 32 * 1024;
			await ReadStreamAsync(fileSection.FileStream, bufferSize);
			fileSection.FileStream.Position = 0;
			var contentType = section.ContentType ?? "application/octet-stream";
			var formFile = new DisposableFormFile(fileSection.FileStream, baseStreamOffset: 0, fileSection.FileStream.Length, fileSection.Name, fileSection.FileName)
			{
				Headers = new HeaderDictionary(),
				ContentType = contentType,
				ContentDisposition = section.ContentDisposition ?? ""
			};

			return formFile;
		}

		private static async Task ReadStreamAsync(Stream stream, int bufferSize)
		{
			var buffer = new byte[bufferSize];
			int bytesRead;

			do
			{
				bytesRead = await stream.ReadAsync(buffer.AsMemory(start: 0, bufferSize));
			} while (bytesRead > 0);
		}
    }
}