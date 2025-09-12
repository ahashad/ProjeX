using System;
using System.Threading.Tasks;
using LastMinute.Consultancy.Application.ChangeRequest.Commands;

namespace LastMinute.Consultancy.Application.ChangeRequest
{
    public interface IChangeRequestService
    {
        Task<ChangeRequestDto> RaiseAsync(RaiseChangeRequestCommand command);
        Task<ChangeRequestDto> ProcessAsync(ProcessChangeRequestCommand command);
    }
}

