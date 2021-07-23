$.post("http://localhost:8080/catch", { name: "John", time: "2pm" },
    function(data) { 
        alert(data);
    });