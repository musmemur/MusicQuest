export async function getFileData(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = function (e) {
            const result = e.target?.result;
            if (result && typeof result === "string") {
                const mimeType = file.type;
                const base64Data = result.split(",")[1];
                resolve(`data:${mimeType};base64,${base64Data}`);
            } else {
                reject(new Error("Ошибка чтения файла"));
            }
        };
        reader.onerror = function () {
            reject(new Error("Ошибка чтения файла."));
        };
        reader.readAsDataURL(file);
    });
}