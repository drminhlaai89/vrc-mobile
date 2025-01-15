<?php
if ($handle = opendir('.')) {

    while (false !== ($entry = readdir($handle))) {

        if ($entry != "." && $entry != ".." && $entry !=".htaccess" && $entry != "GetAllFile.php") {

            echo "$entry|";
        }
        
    }

    closedir($handle);
}
?>