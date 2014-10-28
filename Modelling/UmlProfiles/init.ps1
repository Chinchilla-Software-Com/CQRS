param($installPath, $toolsPath, $package, $project)

$parentFolder = resolve-path "$package\..";
$templatesFolder = resolve-path "$toolsPath\..\Templates";

$global:solutionScriptsContainer = Join-Path $parentFolder "ModelingTemplates"

function global:Update-ModelingTemplates()
{		
	if(!(test-path $solutionScriptsContainer -pathtype container)) 
	{
		new-item $solutionScriptsContainer -type directory
	}

	$files = Get-ChildItem $templatesFolder

	foreach ($file in $files)
	{	
		if ($file.extension -eq ".t4")
		{
			copy $file.fullname $solutionScriptsContainer
		}
	}
}

Update-ModelingTemplates

$visxPath = resolve-path "$toolsPath\..\VisualStudioAddOns\Cqrs.Modelling.UmlProfiles.vsix";
START $visxPath