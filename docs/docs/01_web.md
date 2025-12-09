# Web UI
STaaS can be accessed using a Web UI

## Signing
Artifact signature can be simply executed by following these steps:

1. Press the `Sign` button
1. Select a file to sign and optionally provide a comment

In the background, the sign page calculates the digest of the selected file and
submits it for signature. Signed files can be viewed by pressing the `Activity`
button. From there, you can download the signature bundle and you can view information
about the generated certificate, as well as, the record stored in the public registry.
Signatures can be deleted from STaaS but they are not revoked. 

## Verification
Generated bundles can be verified using the [Cosign tool](https://docs.sigstore.dev/signing/quickstart/).
For this verification you would need STaaS's CA certificate. This can be obtained
by clicking [here](https://staas.excid.io/Sign/Certificate).  A signature bundle can be
verified using the following command:

```
cosign verify-blob \
    --certificate-identity=YOUR_STAAS_IDENTITY \
    --certificate-oidc-issuer=https://staas.excid.io \
    --certificate-chain ca.pem \
    --insecure-ignore-sct \
    --bundle signature.bundle \
    YOUR_FILE
```
The `--insecure-ignore-sct` flag is required since certificated generated using a private
instance of Fulcio are not allowed to be recorded in the transparency registry.
