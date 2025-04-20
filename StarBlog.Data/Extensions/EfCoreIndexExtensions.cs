using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StarBlog.Data.Extensions;

public static class EfCoreIndexExtensions {
    /// <summary>
    /// 嵌套值对象的索引
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="ownedNavigation"></param>
    /// <param name="propertyExpression"></param>
    /// <param name="indexName"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOwned"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <returns></returns>
    public static IndexBuilder HasNestedOwnedIndex<T, TOwned, TProperty>(
        this EntityTypeBuilder<T> builder,
        Expression<Func<T, TOwned>> ownedNavigation,
        Expression<Func<TOwned, TProperty>> propertyExpression,
        string? indexName = null)
        where T : class
        where TOwned : class {
        var ownedPropertyName = GetPropertyName(ownedNavigation);
        var propertyName = GetPropertyName(propertyExpression);
        var columnName = $"{ownedPropertyName}_{propertyName}";

        var index = builder.HasIndex(columnName);

        if (!string.IsNullOrWhiteSpace(indexName)) {
            index.HasDatabaseName(indexName);
        }

        return index;
    }

    /// <summary>
    /// 批量配置多个嵌套值对象的索引
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="ownedNavigation"></param>
    /// <param name="indexConfigurations"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOwned"></typeparam>
    public static void HasMultipleNestedOwnedIndexes<T, TOwned>(
        this EntityTypeBuilder<T> builder,
        Expression<Func<T, TOwned>> ownedNavigation,
        IEnumerable<(Expression<Func<TOwned, object>> propertyExpression, string indexName)> indexConfigurations)
        where T : class
        where TOwned : class {
        foreach (var (propertyExpression, indexName) in indexConfigurations) {
            var propertyName = GetPropertyName(propertyExpression);
            var ownedPropertyName = GetPropertyName(ownedNavigation);
            var columnName = $"{ownedPropertyName}_{propertyName}";

            builder.HasIndex(columnName)
                .HasDatabaseName(indexName);
        }
    }


    private static string GetPropertyName(LambdaExpression expression) {
        if (expression.Body is MemberExpression memberExpression) {
            // 转换为 snake_case 格式
            var name = memberExpression.Member.Name;
            return name.ToSnakeCase();
        }

        throw new ArgumentException("Invalid expression");
    }

    /// <summary>
    /// 将 CamelCase 转换为 snake_case
    /// </summary>
    private static string ToSnakeCase(this string input) {
        if (string.IsNullOrEmpty(input)) {
            return input;
        }

        var startUnderscores = Regex.Match(input, @"^_+");
        return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }
}