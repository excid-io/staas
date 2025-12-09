# Introduction
This is the documentation of [Software Transparency as a Service-STaaS](https://staas.excid.io)
platform. You can use STaaS for free [here](https://staas.excid.io). 
You can access the source code of STaaS platform at [GitHub](https://github.com/excid-io/staas).

## Overview
STaaS platform is a free, open-source platform for signing artifacts. 
Signatures are generated using an one-time signing key. The corresponding public
key is included in a short-lived certificate. This certificate also includes the
identity of the user in the STaaS platform. User authentication in STaaS platform
is implemented using OpenID Connect. Signatures are recorded in a public, auditable
registry.

## Technology stack
Short-lived certificates are generated using a private instance 
of [Fulcio CA](https://docs.sigstore.dev/certificate_authority/overview/).
Signatures are recorded in the public instance of [Rekor](https://docs.sigstore.dev/logging/overview/).
STaaS generates a signature bundle that can be verified using
[Cosign](https://docs.sigstore.dev/signing/quickstart/).

