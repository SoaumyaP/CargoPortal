using FluentValidation;
namespace Groove.SP.Application.Common
{
    /// <summary>
    /// Base Validation Class Base
    /// </summary>
    /// <typeparam name="T">type of BaseViewModel</typeparam>
    /// <seealso cref="FluentValidation.AbstractValidator{T}" />
    public abstract class BaseValidation<T> : AbstractValidator<T> where T : class
    {
    }
}
