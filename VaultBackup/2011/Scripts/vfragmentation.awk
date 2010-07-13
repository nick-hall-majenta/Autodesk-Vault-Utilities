BEGIN { 
 totalCounts = 0 
 abovethreshold = 0 
 belowthreshold = 0 
 totalpercent = 0 
 fragmentationthreshold = 75
 fragmentationaveragethreshold = 5
 defrag = 0
 }

/Fragment/ { 
 totalCounts++; 
 percent = $2;
 sub(/%/, "", percent)
 totalpercent +=  percent
 if ( percent > fragmentationthreshold) {
 abovethreshold++; 
 }
else {
 belowthreshold++; 
 }
}

END { 
 print "Autodesk Vault Fragmentation"
 print "--------------------------------------------------------------" 
 print "Number of tables                               : " totalCounts 
 print "Fragmentation threshold                        : " fragmentationthreshold
 print "Fragmentation average threshold                : " fragmentationaveragethreshold
 print ""
 print "Number of tables above fragmentation threshold : " abovethreshold 
 print "Number of tables below threshold               : " belowthreshold 
 print "Average fragmentation (%)                      : " totalpercent / totalCounts
 print ""
 if (abovethreshold / totalCounts > fragmentationaveragethreshold) {
  print "Number of fragmented tables above threshold"
   defrag = 1
 }
 if (totalpercent / totalCounts > fragmentationaveragethreshold) {
  print "Average fragmentation above threshold"
   defrag = 1
 }
 if ( defrag == 1) {
  print "Defragmentation recommended"  
 }
 print ""
}  