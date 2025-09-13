using System;
using System.Threading.Tasks;
using ProjeX.Application.ChangeRequest.Commands;

namespace ProjeX.Application.ChangeRequest
{
    public interface IChangeRequestService
    {
        Task<ChangeRequestDto> RaiseAsync(RaiseChangeRequestCommand command);
        Task<ChangeRequestDto> ProcessAsync(ProcessChangeRequestCommand command);
    }
}

