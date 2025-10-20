import React, { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";

function UploadResult(){
    const location = useLocation();
    const navigate = useNavigate();
    const { apiUrl, challengeId } = location.state || {};
    const [uploadType, setUploadType] = useState("");
    const [freeText, setFreeText] = useState("");
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
            } else if (uploadType === "text") {
                formData.append("freeText", freeText);
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
        <div className="upload-container">
            <div className="upload-card">
                <h2 className="section-title">Upload Your Result</h2>

                <div className="upload-type-toggle" role="tablist" aria-label="Choose upload type">
                    <button
                        className={`btn toggle-btn ${uploadType === "youtube" ? "active" : ""}`}
                        onClick={() => setUploadType("youtube")}
                        role="tab"
                        aria-selected={uploadType === "youtube"}
                    >
                        YouTube Link
                    </button>
                    <button
                        className={`btn toggle-btn ${uploadType === "file" ? "active" : ""}`}
                        onClick={() => setUploadType("file")}
                        role="tab"
                        aria-selected={uploadType === "file"}
                    >
                        Upload File
                    </button>
                    <button
                        className={`btn toggle-btn ${uploadType === "text" ? "active" : ""}`}
                        onClick={() => setUploadType("text")}
                        role="tab"
                        aria-selected={uploadType === "text"}
                    >
                        Free text
                    </button>
                </div>

                {uploadType === "youtube" && (
                    <div className="form-group">
                        <label htmlFor="yt-url" className="form-label">Paste YouTube link</label>
                        <input
                            id="yt-url"
                            type="url"
                            className="input"
                            placeholder="https://youtu.be/..."
                            value={youtubeLink}
                            onChange={e => setYoutubeLink(e.target.value)}
                        />
                    </div>
                )}

                {uploadType === "file" && (
                    <div className="form-group">
                        <label htmlFor="file-input" className="form-label">Choose a file</label>
                        <input
                            id="file-input"
                            type="file"
                            accept="video/*,image/*,application/pdf"
                            onChange={handleFileChange}
                            className="input-file"
                        />
                        {file && (
                            <div className="file-pill" title={file.name}>
                                {file.name} <span className="file-size">({(file.size / 1024 / 1024).toFixed(2)} MB)</span>
                            </div>
                        )}
                    </div>
                )}

                {uploadType === "text" && (
                    <div className="form-group">
                        <label htmlFor="free-text" className="form-label">Free text</label>
                        <textarea
                            id="free-text"
                            className="textarea"
                            placeholder="Write your description or text result..."
                            value={freeText}
                            onChange={e => setFreeText(e.target.value)}
                        />
                    </div>
                )}

                {uploadType && (
                    <button
                        className="btn btn-primary submit-btn"
                        onClick={handleUpload}
                        disabled={
                            uploading ||
                            (uploadType === "file" && !file) ||
                            (uploadType === "youtube" && !youtubeLink) ||
                            (uploadType === "text" && !freeText)
                        }
                    >
                        {uploading ? "Uploading..." : "Submit"}
                    </button>
                )}

                {message && (
                    <div className={`status-msg ${message.startsWith("Failed") ? "error" : "success"}`}>
                        {message}
                    </div>
                )}
            </div>
        </div>
    );
}
export default UploadResult;