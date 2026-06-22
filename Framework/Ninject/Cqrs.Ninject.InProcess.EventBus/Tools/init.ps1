param($installPath, $toolsPath, $package)

# open CS-CQRS splash page on package install
# don't open if the package is installed as a dependency

try
{
  $clientPublicKey = "54BDD3F2F0864FE3B700127FC33714D7";
  $url = "Unknown/Unknown/Unknown"
  $SafePassword = "Unknown"
  try
  {
    $url = $package.Id
  }
  catch{
    $url = "Unknown"
  }
  try
  {
    $url = ($url + "/" + $package.Version)
  }
  catch{
    $url = $url + "/Unknown"
  }
  $url = $url + "/Unknown"
   try
  {
    $url = ($url + "/" + $dte.Solution.FullName)
  }
  catch{
    $url = $url + "/Unknown"
  }

  try
  {
    $url = ($url + "/" + $dte.Name)
  }
  catch{
    $url = $url + "/Unknown"
  }
  try
  {
    $url = ($url + "/" + $dte.Version)
  }
  catch{
    $url = $url + "/Unknown"
  }
  try
  {
    $url = ($url + "/" + $dte.Edition)
  }
  catch{
    $url = $url + "/Unknown"
  }

  try
  {
    $Password = $url | ConvertTo-SecureString -AsPlainText -Force
    $Key = (3,4,2,3,56,34,254,222,1,1,2,23,42,54,33,233,1,34,2,7,6,5,35,43)
    $SafePassword = $Password | ConvertFrom-SecureString -key $key
  }
  catch{}

  Add-Type -AssemblyName System.Web
  $url = "https://api.analytics.chinchillasoftware.com/" + $clientPublicKey + "/NuGet/Install/" + [System.Web.HttpUtility]::UrlEncode($package.Id) + "/?noContent=true&track=" + [System.Web.HttpUtility]::UrlEncode($SafePassword)

  # echo $url

  $response = Invoke-WebRequest -URI $url
}
catch
{
    # stop potential errors from bubbling up
    # worst case the splash page won't open  
}

# still yolo