import React, { useState, useEffect } from 'react';
import axios from '../api/axiosConfig';
import { useNavigate } from 'react-router-dom';
import '../css/CreateProperty.css'; 

const CreateProperty: React.FC = () => {
    const [name, setName] = useState('');
    const [address, setAddress] = useState('');
    const [description, setDescription] = useState('');
    const [features, setFeatures] = useState<string>('');
    const [images, setImages] = useState<File[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    const [userId, setUserId] = useState<string | null>(null);

    useEffect(() => {
        const storedUserId = localStorage.getItem('userId');
        if (storedUserId) {
            setUserId(storedUserId);
        } else {
            setError('User ID is required to create a property.');
        }
    }, []);

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            setImages(Array.from(e.target.files));
        }
    };

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setLoading(true);
        setError(null);

        // Prepare multipart/form-data payload
        const formData = new FormData();
        formData.append('name', name);
        formData.append('address', address);
        formData.append('description', description);
        formData.append('features', features.split(',').map(feature => feature.trim()).join(','));
        if (userId) {
            formData.append('userId', userId);
        }

        // Append each image to the form data
        images.forEach((image) => {
            formData.append(`photos`, image); // Use a consistent key for multiple images
        });

        try {
            await axios.post('/api/Properties', formData, {
                headers: {
                    'Content-Type': 'multipart/form-data',
                },
            });
            alert('Property created successfully!');
            navigate('/'); // Redirect to homepage or property list after creation
        } catch (error) {
            setError('Failed to create property. Please try again.');
            console.error('Error creating property:', error);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="create-container">
            <div className='create-content'>
                <p>Create a New Property</p>
                {error && <p className="error-message">{error}</p>}
                <form onSubmit={handleSubmit}>
                    <label htmlFor="name">Name</label>
                    <input
                        type="text"
                        id="name"
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        placeholder="Enter property name"
                        required
                    />

                    <label htmlFor="address">Address</label>
                    <input
                        type="text"
                        id="address"
                        value={address}
                        onChange={(e) => setAddress(e.target.value)}
                        placeholder="Enter property address"
                        required
                    />

                    <label htmlFor="description">Description</label>
                    <textarea
                        id="description"
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                        placeholder="Enter property description"
                        required
                        className="expanding-textarea"
                    ></textarea>

                    <label htmlFor="features">Features (comma separated)</label>
                    <textarea
                        id="features"
                        value={features}
                        onChange={(e) => setFeatures(e.target.value)}
                        placeholder="e.g., Pool, Garden, Parking"
                        required
                    ></textarea>

                    <label htmlFor="images">Upload Images</label>
                    <input
                        type="file"
                        id="images"
                        multiple
                        accept="image/*"
                        onChange={handleImageChange}
                    />

                    <div className='button-group'>
                        <button type="submit" disabled={loading}>{loading ? 'Creating...' : 'Create Property'}</button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default CreateProperty;
