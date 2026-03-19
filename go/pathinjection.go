package main

import (
	"fmt"
	"log"
	"net/http"
	"os"
	"path/filepath"
)

func handler(w http.ResponseWriter, r *http.Request) {
	pathParam := r.URL.Query().Get("path")
	if pathParam == "" {
		http.Error(w, "Missing 'path' query parameter", http.StatusBadRequest)
		return
	}

	// BAD: This could read any file on the file system
	data, err := os.ReadFile(pathParam)
	if err != nil {
		http.Error(w, "Error reading file: "+err.Error(), http.StatusInternalServerError)
		return
	}
	w.Write(data)
}

func handlerJoin(w http.ResponseWriter, r *http.Request) {
	pathParam := r.URL.Query().Get("path")
	if pathParam == "" {
		http.Error(w, "Missing 'path' query parameter", http.StatusBadRequest)
		return
	}

	// BAD: This could still read any file on the file system
	data, err := os.ReadFile(filepath.Join("/home/user/", pathParam))
	if err != nil {
		http.Error(w, "Error reading file: "+err.Error(), http.StatusInternalServerError)
		return
	}
	w.Write(data)
}

func main() {
	http.HandleFunc("/read", handler)
	http.HandleFunc("/readjoin", handlerJoin)
	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		fmt.Fprintln(w, "Path Injection Demo Server")
		fmt.Fprintln(w, "Try: GET /read?path=/etc/hostname")
		fmt.Fprintln(w, "Try: GET /readjoin?path=../../etc/hostname")
	})

	port := "8080"
	log.Printf("Path injection demo server listening on http://localhost:%s\n", port)
	log.Fatal(http.ListenAndServe(":"+port, nil))
}
