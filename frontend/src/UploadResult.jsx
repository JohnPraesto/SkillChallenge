import React, { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";

function UploadResult(){
    const location = useLocation();
    const navigate = useNavigate();
    const { apiUrl, challengeId } = location.state || {};
    const [uploadType, setUploadType] = useState("");
    const [file, setFile] = useState(null);
    const [youtubeLink, setYoutubeLink] = useState("");
    const [uploading, setUploading] = useState(false);
    const [message, setMessage] = useState("");

    const handleFileChange = (e) => {
        setFile(e.target.files[0]);
    };

    const handleUpload = async () => {
        setUploading(true);
        setMessage("");
        try {
            let res;
            const formData = new FormData();
            if (uploadType === "youtube") {
                formData.append("youtubeUrl", youtubeLink);
            } else if (uploadType === "file" && file) {
                formData.append("file", file);
            }
            res = await fetch(
                `${apiUrl}/api/challenges/${challengeId}/upload-result`,
                {
                    method: "POST",
                    headers: {
                        "Authorization": `Bearer ${localStorage.getItem("token")}`,
                    },
                    body: formData,
                }
            );
             if (res && res.ok) {
                setMessage("Result uploaded!");
                setTimeout(() => navigate && navigate(-1), 1000);
            } else {
                const text = res ? await res.text() : "No response";
                setMessage("Failed to upload result: " + text);
            }
        } catch (err) {
            setMessage("Failed to upload result: " + err.message);
        }
        setUploading(false);
    };
    
    return (
        <div>
            <h2>Upload Your Result</h2>
            <div style={{ marginBottom: 20 }}>
                <button
                    className={uploadType === "youtube" ? "btn btn-primary" : "btn"}
                    style={{ marginRight: 10 }}
                    onClick={() => setUploadType("youtube")}
                >
                    Youtube Link
                </button>
                <button
                    className={uploadType === "file" ? "btn btn-primary" : "btn"}
                    onClick={() => setUploadType("file")}
                >
                    Upload File
                </button>
            </div>

            {uploadType === "youtube" && (
                <div>
                    <input
                        type="text"
                        placeholder="Paste Youtube link here"
                        style={{ width: 300 }}
                        value={youtubeLink}
                        onChange={e => setYoutubeLink(e.target.value)}
                    />
                </div>
            )}
            {uploadType === "file" && (
                <div>
                    <input
                        type="file"
                        accept="video/*,image/*,application/pdf"
                        onChange={handleFileChange}
                    />
                </div>
            )}

            {uploadType && (
                <button
                    className="btn btn-success"
                    style={{ marginTop: 20 }}
                    onClick={handleUpload}
                    disabled={uploading || (uploadType === "file" && !file) || (uploadType === "youtube" && !youtubeLink)}
                >
                    {uploading ? "Uploading..." : "Submit"}
                </button>
            )}

            {message && <div style={{ marginTop: 12 }}>{message}</div>}
        </div>
    );
}
export default UploadResult;