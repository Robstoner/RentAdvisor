import React, { useState } from 'react';
import axios from '../api/axiosConfig';
import '../css/CreateProperty.css';
import { useNavigate } from 'react-router-dom';

const CreateProperty: React.FC = () => {
    const [name, setName] = useState('');
    const [address, setAddress] = useState('');
    const [description, setDescription] = useState('');
    const [features, setFeatures] = useState<string>('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setLoading(true);
        setError(null);

        const propertyData = {
            name,
            address,
            description,
            features: features.split(',').map(feature => feature.trim())
        };

        try {
            await axios.post('/api/Properties', propertyData);
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
                    <input
                        type="text"
                        id="features"
                        value={features}
                        onChange={(e) => setFeatures(e.target.value)}
                        placeholder="e.g., Pool, Garden, Parking"
                        required
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
