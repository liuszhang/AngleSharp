﻿namespace AngleSharp.Css.Dom
{
    using AngleSharp.Dom;
    using AngleSharp.Text;
    using System;
    using System.IO;

    /// <summary>
    /// Represents a simple selector (either a type selector, universal
    /// selector, attribute, class, id or pseudo-class selector).
    /// </summary>
    sealed class SimpleSelector : ISelector
    {
        #region Fields

        private readonly Func<IElement, IElement, Boolean> _matches;
        private readonly Priority _specificity;
        private readonly String _code;

        #endregion

        #region ctor

        public SimpleSelector()
            : this(_ => true, Priority.Zero, "*")
        {
        }

        public SimpleSelector(String match)
            : this(el => match.Isi(el.LocalName), Priority.OneTag, match)
        {
        }

        public SimpleSelector(Predicate<IElement> matches, Priority specifify, String code)
            : this((e, _) => matches(e), specifify, code)
        {
        }

        public SimpleSelector(Func<IElement, IElement, Boolean> matches, Priority specifify, String code)
        {
            _matches = matches;
            _specificity = specifify;
            _code = code;
        }

        #endregion

        #region Properties

        public static readonly SimpleSelector All = new SimpleSelector();

        public Priority Specificity
        {
            get { return _specificity; }
        }

        public String Text
        {
            get { return _code; }
        }

        #endregion

        #region Static constructors

        public static SimpleSelector PseudoElement(Predicate<IElement> action, String pseudoElement)
        {
            return new SimpleSelector(action, Priority.OneTag, PseudoElementNames.Separator + pseudoElement);
        }

        public static SimpleSelector PseudoClass(Predicate<IElement> action, String pseudoClass)
        {
            return new SimpleSelector(action, Priority.OneClass, PseudoClassNames.Separator + pseudoClass);
        }

        public static SimpleSelector PseudoClass(Func<IElement, IElement, Boolean> action, String pseudoClass)
        {
            return new SimpleSelector(action, Priority.OneClass, PseudoClassNames.Separator + pseudoClass);
        }

        public static SimpleSelector Class(String match)
        {
            return new SimpleSelector(_ => _.ClassList.Contains(match), Priority.OneClass, "." + match);
        }

        public static SimpleSelector Id(String match)
        {
            return new SimpleSelector(_ => _.Id.Is(match), Priority.OneId, "#" + match);
        }

        public static SimpleSelector AttrAvailable(String match, String prefix = null)
        {
            var front = match;

            if (!String.IsNullOrEmpty(prefix))
            {
                front = FormFront(prefix, match);
                match = FormMatch(prefix, match);
            }

            var code = FormCode(front);
            return new SimpleSelector(_ => _.HasAttribute(match), Priority.OneClass, code);
        }

        public static SimpleSelector AttrMatch(String match, String value, String prefix = null)
        {
            var front = match;

            if (!String.IsNullOrEmpty(prefix))
            {
                front = FormFront(prefix, match);
                match = FormMatch(prefix, match);
            }

            var code = FormCode(front, "=", value.CssString());
            return new SimpleSelector(_ => _.GetAttribute(match).Is(value), Priority.OneClass, code);
        }

        public static SimpleSelector AttrNotMatch(String match, String value, String prefix = null)
        {
            var front = match;

            if (!String.IsNullOrEmpty(prefix))
            {
                front = FormFront(prefix, match);
                match = FormMatch(prefix, match);
            }

            var code = FormCode(front, "!=", value.CssString());
            return new SimpleSelector(_ => _.GetAttribute(match) != value, Priority.OneClass, code);
        }

        public static SimpleSelector AttrList(String match, String value, String prefix = null)
        {
            var front = match;

            if (!String.IsNullOrEmpty(prefix))
            {
                front = FormFront(prefix, match);
                match = FormMatch(prefix, match);
            }

            var code = FormCode(front, "~=", value.CssString());
            var matches = Select(value, _ => (_.GetAttribute(match) ?? String.Empty).SplitSpaces().Contains(value));
            return new SimpleSelector(matches, Priority.OneClass, code);
        }

        public static SimpleSelector AttrBegins(String match, String value, String prefix = null)
        {
            var front = match;

            if (!String.IsNullOrEmpty(prefix))
            {
                front = FormFront(prefix, match);
                match = FormMatch(prefix, match);
            }

            var code = FormCode(front, "^=", value.CssString());
            var matches = Select(value, _ => (_.GetAttribute(match) ?? String.Empty).StartsWith(value));
            return new SimpleSelector(matches, Priority.OneClass, code);
        }

        public static SimpleSelector AttrEnds(String match, String value, String prefix = null)
        {
            var front = match;

            if (!String.IsNullOrEmpty(prefix))
            {
                front = FormFront(prefix, match);
                match = FormMatch(prefix, match);
            }

            var code = FormCode(front, "$=", value.CssString());
            var matches = Select(value, _ => (_.GetAttribute(match) ?? String.Empty).EndsWith(value));
            return new SimpleSelector(matches, Priority.OneClass, code);
        }

        public static SimpleSelector AttrContains(String match, String value, String prefix = null)
        {
            var front = match;

            if (!String.IsNullOrEmpty(prefix))
            {
                front = FormFront(prefix, match);
                match = FormMatch(prefix, match);
            }

            var code = FormCode(front, "*=", value.CssString());
            var matches = Select(value, _ => (_.GetAttribute(match) ?? String.Empty).Contains(value));
            return new SimpleSelector(matches, Priority.OneClass, code);
        }

        public static SimpleSelector AttrHyphen(String match, String value, String prefix = null)
        {
            var front = match;

            if (!String.IsNullOrEmpty(prefix))
            {
                front = FormFront(prefix, match);
                match = FormMatch(prefix, match);
            }
            
            var code = FormCode(front, "|=", value.CssString());
            var matches = Select(value, _ => (_.GetAttribute(match) ?? String.Empty).HasHyphen(value));
            return new SimpleSelector(matches, Priority.OneClass, code);
        }

        public static SimpleSelector Type(String match)
        {
            return new SimpleSelector(match);
        }

        #endregion

        #region Methods

        public Boolean Match(IElement element, IElement scope)
        {
            return _matches(element, scope);
        }

        public void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            writer.Write(Text);
        }

        #endregion

        #region Helpers

        private static Predicate<IElement> Select(String value, Predicate<IElement> predicate)
        {
            return String.IsNullOrEmpty(value) ? (_ => false) : predicate;
        }

        private static String FormCode(String content)
        {
            return String.Concat("[", content, "]");
        }

        private static String FormCode(String name, String op, String value)
        {
            var content = String.Concat(name, op, value);
            return FormCode(content);
        }

        private static String FormFront(String prefix, String match)
        {
            return String.Concat(prefix, CombinatorSymbols.Pipe, match);
        }

        private static String FormMatch(String prefix, String match)
        {
            return prefix.Is("*") ? match : String.Concat(prefix, PseudoClassNames.Separator, match);
        }

        #endregion
    }
}
