运行Glash代理端
```
docker run\
 --name glash-agent\
 -d\
 --restart=always\
 -p 21001:80\
 aaasoft/glash-agent:20231016
```

参数说明
```
默认密码：admin
```