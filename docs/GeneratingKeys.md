# Generating an SSH Key (Windows 10)

## Steps

The following steps are options if you wish to use an SSH Private Key. These steps were written for Windows 10, however, on Linux the steps are similar.

1. Open PowerShell:
2. **Generate key** (_with old PEM format_)
   1. `ssh-keygen -m PEM -t rsa -b 4096`
   2. In the future, we'll be able to use `ssh-keygen`.. just not yet.
3. Set output name (_default is okay for basic setups_)
4. Input a passphrase for the key _(OPTIONAL)_
5. Windows will now generate your RSA public/private key pair.
   1. Default location: `%UserProfile%\.ssh` (WINOWS)
   2. The public key will be stored as `id_rsa.pub` in the directory
6. **Upload the public key** to your remote machine
   1. Navigate to folder, `~/.ssh/` on Linux device
   2. If `~/.ssh/authorized_keys` exists, append the contents of `id_rsa.pub` to the next line.
   3. If it does not exist, simply upload `id_rsa.pub` and rename it to, `authorized_keys`
7. Test your connection using SSH on Windows via `ssh user@hostname`

## Convert Key to PEM format

SSH.Net still has some issues with ssh-rsa. To overcome this, you'll need to convert keyfile to PEM.

```powershell
ssh-keygen -p -P "OLD_PASSPHRASE" -N "NEW_PASSPHRASE" -m pem -f "%UserProfile%\.ssh\id_rsa"
```

## Sample output

```cmd
C:\workXXXXXX> ssh-keygen -m PEM -t rsa -b 4096
Generating public/private rsa key pair.
Enter file in which to save the key (C:\Users\XXXXX/.ssh/id_rsa):
Enter passphrase (empty for no passphrase):
Enter same passphrase again:
Your identification has been saved in C:\Users\XXXXXX/.ssh/id_rsa.
Your public key has been saved in C:\Users\XXXXX/.ssh/id_rsa.pub.
The key fingerprint is:
SHA256:ETNWXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXcms YYYYYYY\XXXXX@ZZZZZZZZ
The key's randomart image is:
+---[RSA 3072]----+
|       oO=o      |
| XXXXXXXXXXXX    |
| XXXXXXXXXXXX    |
| XXXXXXXXXXXX    |
| XXXXXXXXXXXX    |
|+XXXXXXXXXXXX    |
|.XXXXXXXXXXXX    |
|oXXXXXXXXXXXX    |
|o+..             |
+----[SHA256]-----+
```

## Reference

* [https://www.onmsft.com/how-to/how-to-generate-an-ssh-key-in-windows-10]
