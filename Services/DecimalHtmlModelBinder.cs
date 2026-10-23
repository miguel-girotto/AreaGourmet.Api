using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AreaGourmet.Api.Services;

// HTML <input type="number"> envia ponto decimal, inclusive em Windows pt-BR.
// Não aceitar separadores de milhar evita transformar silenciosamente 12,50 em 1250.
public sealed class DecimalHtmlModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext context)
    {
        var result = context.ValueProvider.GetValue(context.ModelName);
        if (result == ValueProviderResult.None) return Task.CompletedTask;
        context.ModelState.SetModelValue(context.ModelName, result);
        var text = result.FirstValue;
        if (string.IsNullOrEmpty(text) && Nullable.GetUnderlyingType(context.ModelType) is not null)
            context.Result = ModelBindingResult.Success(null);
        else if (decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
            context.Result = ModelBindingResult.Success(number);
        else
            context.ModelState.TryAddModelError(context.ModelName, "Informe um número válido, sem separador de milhar.");
        return Task.CompletedTask;
    }
}
public sealed class DecimalHtmlModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
        context.Metadata.ModelType == typeof(decimal) || context.Metadata.ModelType == typeof(decimal?) ? new DecimalHtmlModelBinder() : null;
}
