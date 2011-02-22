
# Example backup start
# 15/02/2011 19:01:43 -Obackup -B"D:\AutodeskVault\backup\A" -VU[username] -VP[password] -S -DBSC -L"D:\AutodeskVault\backup\BackupA.log" 
/^.*-Obackup.*$/ {
print $0;
}

# Example backup end
# 15/02/2011 21:01:59 The backup operation has been successfully finished.
/^.*The backup operation has been successfully finished.*$/ {
print $0;
}
