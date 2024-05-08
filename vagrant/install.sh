yum update -y
# Cài đặt máy chủ Apache
sudo yum -y install httpd mod_ssl mod_rewrite

# Cài đặt Nginx
echo '
[nginx]
name=nginx repo
baseurl=http://nginx.org/packages/mainline/centos/7/$basearch/
gpgcheck=0
enabled=1
' > /etc/yum.repos.d/nginx.repo

sudo yum install nginx -y

# Tat SELinux cua CentOS
setenforce 0
sed -i --follow-symlinks 's/^SELINUX=enforcing/SELINUX=disabled/' /etc/sysconfig/selinux

# Đổi root password thành 123 và cho phép login SSH qua root
echo "123" | passwd --stdin root
sed -i 's/^PasswordAuthentication no/PasswordAuthentication yes/' /etc/ssh/sshd_config
systemctl reload sshd

# cài .net8 sdk
# net 6,7
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
sudo yum install dotnet-sdk-7.0 -y
sudo yum install aspnetcore-runtime-7.0  -y

# net 8
# sudo yum install wget -y
# sudo wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
# sudo chmod +x ./dotnet-install.sh
# sudo ./dotnet-install.sh --version latest
# export DOTNET_ROOT=$HOME/.dotnet
# export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools

# Cài đặt MS SQL Server 2022 trên CentOS 7
sudo alternatives --config python
sudo yum install python2 -y
sudo yum install compat-openssl10
sudo alternatives --config python
sudo curl -o /etc/yum.repos.d/mssql-server.repo https://packages.microsoft.com/config/rhel/7/mssql-server-2017.repo
sudo yum install -y mssql-server





