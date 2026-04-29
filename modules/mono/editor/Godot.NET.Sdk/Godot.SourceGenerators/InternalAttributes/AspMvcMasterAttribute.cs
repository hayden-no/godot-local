using System;

namespace JetBrains.Annotations;

/// <summary>
/// ASP.NET MVC attribute. Indicates that the marked parameter is an MVC Master. Use this attribute
/// for custom wrappers similar to <c>System.Web.Mvc.Controller.View(String, String)</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
internal sealed class AspMvcMasterAttribute : Attribute { }
