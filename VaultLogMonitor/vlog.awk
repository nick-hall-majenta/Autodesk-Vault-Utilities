
# Example exception timestamp
# 15/02/2011 09:32:26 *******************************************************************
/^.* .* \*+$/ {
print $0;
}

# Example exception
# Exception: BadAuthenticationToken [300] 
/^Exception: .+$/ {
print $0;
}
