运行Glash服务端
```
docker run\
 --name glash-server\
 -d\
 --restart=always\
 -p 21000:80\
 aaasoft/glash-server:20231016
```

参数说明
```
默认密码：admin
```