using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StarBlog.Api.Helpers;

public class ArrayModelBinder : IModelBinder {
    public Task BindModelAsync(ModelBindingContext bindingContext) {
        if (!bindingContext.ModelMetadata.IsEnumerableType) {
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();
        if (string.IsNullOrWhiteSpace(value)) {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        // 使用反射获取参数类型
        var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeParameters[0];
        
        // 根据参数类型，新建一个类型转换器
        var converter = TypeDescriptor.GetConverter(elementType);

        // 按照 , 分割字符串，
        // 指定 `StringSplitOptions.RemoveEmptyEntries` 参数，用以清除空值，比如 `1,,3,4` -> `["1", "3", "4"]`
        var values = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => converter.ConvertFromString(x.Trim()))
            .ToArray();

        var typedValues = Array.CreateInstance(elementType, values.Length);
        values.CopyTo(typedValues, 0);
        bindingContext.Model = typedValues;

        bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);

        return Task.CompletedTask;
    }
}