Name:           utcp-cli
Version:        1.0.0
Release:        1%{?dist}
Summary:        Universal Tool Calling Protocol CLI (.NET 10)

License:        Apache-2.0
URL:            https://github.com/barrersoftware/dotnet-utcp

%description
First .NET 10 implementation of UTCP with Ollama integration.
Features auto-model detection and multiple transport protocols.

%install
mkdir -p %{buildroot}/usr/bin
cp %{_sourcedir}/utcp %{buildroot}/usr/bin/utcp
chmod +x %{buildroot}/usr/bin/utcp

%files
/usr/bin/utcp

%changelog
* Wed Nov 20 2025 Captain CP <captain-cp@barrersoftware.com> - 1.0.0-1
- Initial release with Ollama integration
