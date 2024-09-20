$certPass = "dev@123"
$certSubj = "scaleup.fin"
$certAltNames = "DNS:localhost,DNS:host.docker.internal,DNS:scaleup.services.communicationapi,DNS:scaleup.services.companyapi,DNS:scaleup.services.identityapi,DNS:scaleup.services.kycapi,DNS:scaleup.services.leadapi,DNS:scaleup.services.locationapi,DNS:scaleup.services.mediaapi,DNS:scaleup.services.nbfcapi,DNS:scaleup.services.ledgerapi,DNS:scaleup.services.loanaccountapi,DNS:scaleup.services.productapi,DNS:scaleup.apigateways.ocelotgw,DNS:scaleup.apigateways.aggregator" # i believe you can also add individual IP addresses here like so: IP:127.0.0.1
$opensslPath="C:\Program Files\Git\usr\bin" #assuming you can download OpenSSL, I believe no installation is necessary
$workDir= Split-Path -parent $MyInvocation.MyCommand.Definition # i assume this will be your solution root

#generate a self-signed cert with multiple domains
Start-Process -NoNewWindow -Wait -FilePath (Join-Path $opensslPath "openssl.exe") -ArgumentList "req -x509 -nodes -days 365 -newkey rsa:2048 -keyout ",
                                          (Join-Path $workDir scaleupcertdev.key),
                                          "-out", (Join-Path $workDir scaleupcertdev.crt),
                                          "-subj `"/CN=$certSubj`" -addext `"subjectAltName=$certAltNames`""

# this time round we convert PEM format into PKCS#12 (aka PFX) so .net core app picks it up
Start-Process -NoNewWindow -Wait -FilePath (Join-Path $opensslPath "openssl.exe") -ArgumentList "pkcs12 -export -in ", 
                                           (Join-Path $workDir scaleupcertdev.crt),
                                           "-inkey ", (Join-Path $workDir scaleupcertdev.key),
                                           "-out ", (Join-Path $workDir scaleupcertdev.pfx),
                                           "-passout pass:$certPass"



$params = @{
    FilePath = (Join-Path $workDir scaleupcertdev.crt)
    CertStoreLocation = 'Cert:\LocalMachine\Root'
}
Import-Certificate @params