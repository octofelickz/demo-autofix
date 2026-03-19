package main

import (
	"fmt"
	"log"
	"net/http"
	"os"
	"path/filepath"
	"strings"
)

const safeReadDir = "/var/data/"
const safeJoinDir = "/home/user/"

func handler(w http.ResponseWriter, r *http.Request) {
	pathParam := r.URL.Query().Get("path")
	if pathParam == "" {
		http.Error(w, "Missing 'path' query parameter", http.StatusBadRequest)
		return
	}

	// GOOD: ensure the resolved path is within the safe directory
	absPath, err := filepath.Abs(filepath.Join(safeReadDir, pathParam))
	if err != nil || !strings.HasPrefix(absPath+string(filepath.Separator), safeReadDir) {
		http.Error(w, "Invalid path parameter", http.StatusBadRequest)
		return
	}
	data, err := os.ReadFile(absPath)
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

	// GOOD: ensure the resolved path is within the safe directory
	absPath, err := filepath.Abs(filepath.Join(safeJoinDir, pathParam))
	if err != nil || !strings.HasPrefix(absPath+string(filepath.Separator), safeJoinDir) {
		http.Error(w, "Invalid path parameter", http.StatusBadRequest)
		return
	}
	data, err := os.ReadFile(absPath)
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
		fmt.Fprintln(w, "Try: GET /read?path=somefile.txt")
		fmt.Fprintln(w, "Try: GET /readjoin?path=somefile.txt")
	})

	port := "8080"
	log.Printf("Path injection demo server listening on http://localhost:%s\n", port)
	log.Fatal(http.ListenAndServe(":"+port, nil))
}
