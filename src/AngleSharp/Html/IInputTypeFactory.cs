﻿namespace AngleSharp.Html
{
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.InputTypes;
    using System;

    interface IInputTypeFactory
    {
        BaseInputType Create(IHtmlInputElement input, String type);
    }
}
