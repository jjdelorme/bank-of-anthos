# To build:
#
# PROJECT_ID=jasondel-test-project
# VERSION=v0.0.1 && docker build --force-rm -t gcr.io/$PROJECT_ID/overdraft:$VERSION . -f Dockerfile
#
# To run:
# docker run -e ASPNETCORE_URLS="http://*:8080" -p 8080:8080 overdraft:$VERSION
#

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

WORKDIR /overdraft
COPY ./ .

# Run unit tests.
#RUN dotnet test ./test/overdraft.tests.csproj

# Build self-contained executable.
RUN dotnet publish ./src/overdraft.csproj -c Release -o ./build/

FROM mcr.microsoft.com/dotnet/aspnet:5.0 as runtime

# Just for testing now
ENV ASPNETCORE_ENVIRONMENT="Development"

# # Workaround, dependencies for Google Firestore SDK.
RUN apt-get update \
    &&  apt-get install -y --no-install-recommends libgdiplus libc6-dev

WORKDIR /overdraft
COPY --from=build /overdraft/build .

ENTRYPOINT ["dotnet", "overdraft.dll"]