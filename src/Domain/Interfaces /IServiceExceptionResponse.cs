using System.Collections.Generic;

namespace Domain.Interfaces;

public interface IServiceExceptionResponse
{
	string Status { get; set; }
	string Message { get; set; }
	List<string> Errors { get; set; }
}
