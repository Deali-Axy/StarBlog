FROM registry-1.docker.io/library/nginx@sha256:0e52bd054126cdc040b1276c0af347aeb02a41b9cfe305d6094767ac30e97949
COPY admin/ /usr/share/nginx/html/admin/
EXPOSE 8080
