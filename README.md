# Приложение для закрытия заблокированных сетевых файлов любым пользователем, которому выданно разрешение

Приложение добавляет в контекстное меню для файлов пункт "Разблокировать файл" (Shift + ПКМ), при вызове которого произойдет разблокировка сетевого файла, если он был заблокирован.  
Если файл не является сетевым - программа сообщит об этом.  
Если у пользователя нет разрешений на использование данной функции - программа сообщит об этом.  

**CloseOpenFilesApp** - основная логика программы, для работы требуется создать системного пользователя - доменного администратора, зашифровать пароль используя секретный ключ (генерация секретного ключа - ProgGen.cs) 
и "зашить" в код программы имя системного пользователя и зашифрованный пароль. Секретный ключ указывается в файле "confiq.txt" и при установке приложения сохраняется в папку вместе с приложением. При установке для файла "confiq.txt"
накладывается ограничения на чтение.  

**UsersPermissions** - дополнительный функционал по назначению разрешений на чтение файла "confiq.txt". 
Использовать данную подпрограмму и, соответветственно, выдавать разрешение на использование основной программы может только доменный администратор. 

**SetupCloseOpenFilesApp** - WIX установщик приложения. Настройки внесения записи в реестр для добавления пункта в контекстное меню находится в этом же проекте.
