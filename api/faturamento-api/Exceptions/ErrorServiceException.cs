using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace estoque_api.Exceptions
{
    public class ErrorServiceException : Exception
    {
        private readonly Func<ControllerBase, IActionResult> _actionResult;

        public ErrorServiceException(Func<ControllerBase, IActionResult> actionResult, string message = "Erro no serviço.")
        : base(message)
        {
            _actionResult = actionResult;
        }   

        public IActionResult ToActionResult(ControllerBase controller)
        {
            return _actionResult(controller);
        }
    }
}