@echo off
"C:\\Program Files\\Unity\\Hub\\Editor\\6000.0.58f1\\Editor\\Data\\PlaybackEngines\\AndroidPlayer\\OpenJDK\\bin\\java" ^
  --class-path ^
  "C:\\Users\\anvt1\\.gradle\\caches\\modules-2\\files-2.1\\com.google.prefab\\cli\\2.1.0\\aa32fec809c44fa531f01dcfb739b5b3304d3050\\cli-2.1.0-all.jar" ^
  com.google.prefab.cli.AppKt ^
  --build-system ^
  cmake ^
  --platform ^
  android ^
  --abi ^
  x86_64 ^
  --os-version ^
  23 ^
  --stl ^
  c++_shared ^
  --ndk-version ^
  27 ^
  --output ^
  "C:\\Users\\anvt1\\AppData\\Local\\Temp\\agp-prefab-staging11577971813949103638\\staged-cli-output" ^
  "C:\\Users\\anvt1\\.gradle\\caches\\8.11\\transforms\\cf11df13d2fd9a509170edfac64a04e6\\transformed\\jetified-games-activity-3.0.5\\prefab" ^
  "C:\\Users\\anvt1\\.gradle\\caches\\8.11\\transforms\\56cf3a67cfe0327f95b35bbfa69aba26\\transformed\\jetified-games-frame-pacing-1.10.0\\prefab"
