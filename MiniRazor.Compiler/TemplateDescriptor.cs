﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MiniRazor.Utils.Extensions;

namespace MiniRazor;

/// <summary>
/// Represents a compiled Razor template.
/// </summary>
public class TemplateDescriptor
{
    private readonly Type _templateType;

    /// <summary>
    /// Initializes an instance of <see cref="TemplateDescriptor"/>.
    /// </summary>
    public TemplateDescriptor(Type templateType) => _templateType = templateType;

    private ITemplate CreateTemplateInstance() => (ITemplate) (
        Activator.CreateInstance(_templateType) ??
        throw new InvalidOperationException($"Could not instantiate compiled template of type '{_templateType}'.")
    );

    /// <summary>
    /// Renders the template using the specified writer.
    /// </summary>
    public async Task RenderAsync(TextWriter output, object? model, CancellationToken cancellationToken = default)
    {
        var template = CreateTemplateInstance();

        template.Output = output;

        template.Model = model?.GetType().IsAnonymousType() == true
            ? model.ToExpando()
            : model;

        template.CancellationToken = cancellationToken;

        await template.ExecuteAsync();
    }

    /// <summary>
    /// Renders the template to a string.
    /// </summary>
    public async Task<string> RenderAsync(object? model, CancellationToken cancellationToken = default)
    {
        using var output = new StringWriter();
        await RenderAsync(output, model, cancellationToken);

        return output.ToString();
    }
}