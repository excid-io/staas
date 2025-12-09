# API
STaaS provides an API that can be used for signing files.

## Authorization
In order to access STaaS API you need to obtain an authorization token. This can be obtained
by pressing the `API tokens` button. The generated token should be included in all requests
an Authorization header. Here is an example of a python script that uses an API token

```python
headers = {
    'Authorization': 'Basic API_TOKEN'
    }
response = requests.request("POST", "https://staas.excid.io/Api/Sign", headers=headers)
```

## Signing
STaaS API provides a `Sign` endpoint that expects a JSON object that includes the 
following attributes:

* HashBase64: Base64 encoding of a SHA-256 digest.
* Comment : A comment to be stored with the signature.

This is an example of a python script that used the `Sign` API endpoint:

```python
with open(ARTIFACT_TO_SIGN,"rb") as f:
    bytes = f.read() # read entire file as bytes
    artifact_hash = hashlib.sha256(bytes).digest()
headers = {
'Content-Type': 'application/json',
'Authorization': 'Basic API_TOKEN'
}
payload = f"""
{{
    "HashBase64":"{base64.b64encode(artifact_hash).decode()}",
    "Comment":"{item}"  
}}

response = requests.request("POST", url + "Api/Sign", headers=headers, data=payload)
```

If the signing process is successful, the `Sign` endpoint responds with HTTP code
201 and the generated signature bundle.
