//A la hora de guardar y cargar la partida si no se han puesto diferentes item data a las
//notas y llaves, aparecera el objeto que tenga el item data correspondiente, es decir si hay 5 notas y aunque en el item data
//que es data_nota que tiene un espacio para escribir ponemos algo diferente en cada uno,
//no va a funcionar solo funcionara 1 texto en las 5 por lo que tendremos que hacer 5 item data iguales pero de diferente nombre
//y que cada uno vaya con el correspondiente, lo mismo con las llaves.
//Cada uno tendra su nombre y su item data propio (solo llaves y notas para que eso no salgan duplicados o cosas asi, el resto da igual).
//Muy importante que luego los item data se pongan en la funcion todoslositemsdeljuego del script inventorysystem porque luego al guardar
//y cargar no cargara los objetos que lleven esos items
