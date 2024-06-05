# Software Transparency as a Service

## Credits
This project was initially created by [ExcID](https://excid.io) and [Guardtime](https://guardtime.com/)
in the context of the [DISGRID project](https://excid-io.github.io/discgrid/), which was funded by
[SecurIT](https://securit-project.eu/) open call #2. 

## About
This repository includes the source code of our [Software Transparency as a Service](https://staas.excid.io/) platform. 
STaaS uses [Fulcio CA](https://github.com/excid-io/discgrid-dev.git) for generating short-lived certificates
bound to the email of a user. User authentication to the the STaaS platform is implemented
using OpenID connect. Then, STaaS platform acts as an IdP and issues a token, which is 
consumed by Fulcio to generate a signing certificate. Finally, this certificate is used
to sign the provided artifact.

All signatures are recorded in the public instance of [Rekor](https://rekor.sigstore.dev)
and can be verified using [Cosign](https://docs.sigstore.dev/signing/quickstart).

## Preparation
### Fulcio
We will use a self-hosted version of [Fulcio CA](https://github.com/excid-io/discgrid-dev.git)

Particularly, we will use [Fulcio v1.4.4](https://github.com/sigstore/fulcio/releases/tag/v1.4.4)
with [in-disk file](https://github.com/sigstore/fulcio/blob/main/docs/setup.md#on-disk-file) keys. Generate a 
CA certificate and a key using the following command (**make sure you are using a proper password**)

```
 openssl req -x509 \
        -newkey ed25519 \
        -sha256 \
        -keyout fulcio-key.pem \
        -out fulcio-cert.pem \
        -subj "/CN=fulcioCA" \
        -days 36500 \
        -addext basicConstraints=critical,CA:TRUE,pathlen:1 \
        -passout pass:"123456"
```

Then,  modify the configuration in `/etc/fulcio-config/config.json` to include STaaS
platform in its least of IdPs. You should add the following lines (replace 
`https://staas.excid.io` with the URL of you STaaS deployment).

Add the following lines
```
{
    "OIDCIssuers": {
      "https://staas.excid.io": {
        "IssuerURL": "https://staas.excid.io",
        "ClientID": "sigstore",
        "Type": "email"
      }
    }
 }
 ```

Supposedly, the keys generated with the previous step are
stored in `/etc/fulcio-config/` Fulcio can be started using the 
following command (but don't start it yet, STaaS platform
must be executed first). 

```
fulcio serve \
    --port 6002 \
    --ca fileca \
    --fileca-cert=/etc/fulcio-config/fulcio-cert.pem \
    --fileca-key=/etc/fulcio-config/fulcio-key.pem \
    --fileca-key-passwd="123456" \
    --ct-log-url=""
```

### StaaS platform
STaaS platform is implemented using .net 8.0

#### Preparation
STaaS platform includes two configuration files, one for the development environment (`appsettings.Development.json`)
and another for the production environment (`appsettings.Production.json`). You should configure both
of them. Your should configure three sections in the configuration file.

The first section is the `OpenId` section. STaaS uses OpenId connect as an authentication mechanism.
You should provide information about your OpenId provider of preference (e.g., Google, Github, Auth0, or even
an instance of Keycloak).

The second section the is the `IdP` section. STaaS platform generates and signs an `identity token`
used for obtaining a certificate from Fulcio. For this reason you need to generate a signing key.
This key can be generated using OpenSSL using the following command (**make sure you are using a proper password**):

```
openssl ecparam -out ecparam.pem -name prime256v1
openssl genpkey -paramfile ecparam.pem -out idtoken-key.pem -aes-128-cbc -pass pass:"1234564"
```

Configure the IdP section with the path to the generated key and the used password. Moreover,
configure the `iss` parameter with the URL of your deployment. 

The final section is the `Sigstore` section. There, you should provide the URL to 
Fulcio, as well as the path to he Fulcio certificate generated previously. 

As a next step you should prepare STaaS platform's database. This release of STaaS
platform uses SQLite so no additional server needs to be installed. The database
tables are configures using the following procedure (**Note** this procedure will delete 
any existing tables):

From `Staas` folder run:

```
dotnet-ef migrations add InitialCreate
```

If `ef` is not available, install it using  the command `dotnet tool install --global dotnet-ef`

Then execute

```
dotnet ef database update
```


## Excute
First run the STaaS platform .net code by executing from the `STaas` folder

```
dotnet run
```
Then execute Fulcio. Supposedly, Fulcio keys are
stored in `/etc/fulcio-config/` Fulcio can be started using the 
following command:

```
fulcio serve \
    --port 6002 \
    --ca fileca \
    --fileca-cert=/etc/fulcio-config/fulcio-cert.pem \
    --fileca-key=/etc/fulcio-config/fulcio-key.pem \
    --fileca-key-passwd="123456" \
    --ct-log-url=""
```

## Usage
You can upload any file and sign it. Then, you can download the signature bundle and
the certificate of the Fulcio CA and verify the the signature using Cosign using the 
following command  (replace `https://staas.excid.io` with the URL of you STaaS deployment
and excid@example.com with your email):

```
cosign verify-blob \
  --certificate-identity=excid@example.com \
  --certificate-oidc-issuer=https://staas.excid.io \
  --certificate-chain ca.pem \
  --insecure-ignore-sct \
  --bundle signature.bundle FILE
```

You can monitor the public Rekor service for records by STaaS platform 
using [rekor-monitor](https://github.com/sigstore/rekor-monitor)
For example the following commands monitors every 5m the Rekor registry for records made 
by the public instance of STaaS platform on behalf of the email `excid@example.com`.

```
./rekor-monitor/cmd/verifier/verifier -url=https://rekor.sigstore.dev \
  -file="registry.log" \
  -output-identities="identities.log" \
  -interval=5m \
  -monitored-values="
certIdentities:
   - certSubject: excid@example.com
     issuers: 
        - https://staas.excid.io 
" 
```
