using Microsoft.AspNetCore.Mvc;

namespace backend.Services.Errors;

public static class ControllerResultExtensions
{
    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.NoContent();
        }

        var error = result.Error!;

        return controller.Problem(
            title: error.Code.ToString(),
            detail:error.Message,
            statusCode:error.StatusCode
        );
    }

    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Data);
        }

        var error = result.Error!;

        return controller.Problem(
            title: error.Code.ToString(),
            detail: error.Message,
            statusCode: error.StatusCode
        );
    }
}