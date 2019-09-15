# Releasenotes


## [0.1.0] - 2019-09-15
Make a Http GET request with `Dnn.ServicesFramework`:

* ModuleName: Dnn.Resx   
* Url: /Service/get   
* Parameters:   
    * resource - e.g. `DesktopModules/HTML/App_LocalResources/EditHtml.ascx`
    * strategy  - optional, either `Verbatim` or `Underscores`  (default)

Example (for DNN9):   
/API/Dnn.Resx/Service/get?resource=DesktopModules/HTML/App_LocalResources/EditHtml.ascx