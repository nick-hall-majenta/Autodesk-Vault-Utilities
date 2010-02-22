{ 
if ( $4 ~ "^Unsupported data.*" )
  {
  if ( $2 ~ ".*[Dd][Ww][Gg]$" )
    {
    print "%ACAD% \"" $3 "\\" $2 "\"" " %PROFILESWITCH% %PROFILE% %SCRIPTSWITCH% %SCRIPT% "
    }
  }
}