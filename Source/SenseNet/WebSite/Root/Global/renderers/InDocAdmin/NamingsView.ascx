<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="RadaCode.InDoc.Core.Controllers" %>
  
<%
    var routeData = new RouteData();
    routeData.Values["controller"] = "Namings";
    routeData.Values["action"] = "RenderExistingNamings";        
    IController controller = new NamingsController(DependencyResolver.Current.GetService<RadaCode.InDoc.Data.EF.InDocContext>());
    var rc = new RequestContext(new HttpContextWrapper(Context), routeData);
    controller.Execute(rc);        
%>
