$a = New-object -comObject eResept.Forskrivningsmodul.1 -strict
$startPasient = gc -Encoding UTF8 "StartPasient.xml"
$a.StartPasient($startPasient)
